using Core.Constants;
using Core.Interface;
using Core.Models;

namespace Application;

public class ComputeSolution: IComputeSolution
{
    public Response Solution(Payload payload)
    {
        var tempResult = new TemporaryResponseValues(0.0m, payload.Load, 0.0m);
        var sortedPowerplants = payload.GetPowerplantMeritOrder();

        foreach (var plant in sortedPowerplants)
        {
            // Wind powerplants
            if (ProjectConstants.Windturbine.Equals(plant.Type.ToLower())
                && plant.PMax != 0)
            {
                if(ProcessWindTurbinePlant(plant, tempResult)) break;
            }

            // Non-wind powerplants
            else if(!ProjectConstants.Windturbine.Equals(plant.Type.ToLower())
                    && plant.PMax != 0)
            {
                if (ProcessGasAndJetFuelPlant(plant, tempResult)) break;
            }
        }

        CheckOverloadOrUnderload(tempResult, sortedPowerplants);
        tempResult = CompareCostWithoutWind(tempResult, sortedPowerplants);

        // Map to response type
        var result = new Response();
        foreach (var response in tempResult.PowerplantResponses)
        {
            result.PowerplantResponses.Add(response);
        }

        return result;
    }

    private TemporaryResponseValues ReProcessLoadBalancing(TemporaryResponseValues tempResult, List<PowerplantModel> sortedPowerplants)
    {
        if (tempResult.PowerplantResponses.Count == 0)
        {
            return tempResult;
        }
        var unusedPowerplant = GetUnusedPlantInMeritOrder(tempResult, sortedPowerplants);
        var mostUsedPowerplant = GetMostPoweredPlant(tempResult);
        var mostExpensivePowerplant = GetMostExpensivePowerplantUsed(tempResult, sortedPowerplants);

        mostUsedPowerplant.Power -= unusedPowerplant.PMin;
        tempResult.TotalCapacity -= (unusedPowerplant.PMin + mostExpensivePowerplant.Power);
        tempResult.PowerplantResponses.Remove(mostExpensivePowerplant);

        var powerStillNeeded = tempResult.LoadGoal - tempResult.TotalCapacity;
     
        var powerPlantToAdd = new PowerplantResponse(unusedPowerplant.Name, unusedPowerplant.PMin);

        tempResult.TotalCapacity += unusedPowerplant.PMin;
        mostUsedPowerplant.Power += (powerStillNeeded - unusedPowerplant.PMin) ;
        tempResult.TotalCapacity += (powerStillNeeded - unusedPowerplant.PMin);

        tempResult.PowerplantResponses.Add(powerPlantToAdd);

        return tempResult;
    }

    private static PowerplantResponse GetMostPoweredPlant(TemporaryResponseValues response)
    {
        var maxPower = response.PowerplantResponses.Max(p => p.Power);
        return response.PowerplantResponses.First(item => item.Power == maxPower);
    }

    private static PowerplantResponse GetMostExpensivePowerplantUsed(TemporaryResponseValues response, List<PowerplantModel> sortedPowerplants)
    {
        sortedPowerplants.Reverse();
        foreach (var plant in sortedPowerplants)
        { 
           return response.PowerplantResponses.Where(p => p.Name == plant.Name).FirstOrDefault();
        }
        return null;
    }

    private static PowerplantModel GetUnusedPlantInMeritOrder(TemporaryResponseValues response, List<PowerplantModel> sortedPowerplants)
    {
        foreach (var plant in sortedPowerplants)
        {
            if (plant.PMax == 0) continue;
            if (response.PowerplantResponses.All(p => p.Name != plant.Name)) return plant;

        }
        return null;
    }

    private bool ProcessWindTurbinePlant(PowerplantModel plant, TemporaryResponseValues tempResponse)
    {
        {
            tempResponse.TotalCapacity += plant.PMax;

            // underload
            if (tempResponse.TotalCapacity < tempResponse.LoadGoal)
            {
                tempResponse.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax));
                return false;
            }

            // overload
            else if (tempResponse.TotalCapacity > tempResponse.LoadGoal)
            {
                tempResponse.TotalCapacity -= plant.PMax;
                return false;
            }

            // correct load
            else
            {
                tempResponse.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax ));
                return true;
            }
        }
    }

    private bool ProcessGasAndJetFuelPlant(PowerplantModel plant, TemporaryResponseValues tempResponse)
    {

        tempResponse.TotalCapacity += plant.PMin;

        // correct load
        if (tempResponse.TotalCapacity == tempResponse.LoadGoal && plant.PMin != 0)
        {
            tempResponse.TotalCost += plant.Cost * plant.PMin;
            tempResponse.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMin ));
            return true;
        }

        // overload
        else if (tempResponse.TotalCapacity > tempResponse.LoadGoal)
        {
            tempResponse.TotalCapacity -= plant.PMin;
            return false;
        }

        // underload
        else
        {   // If PMin added is less than load, check if PMax can be added. If not, find the power between PMin and PMax for this plant
            return FindPowerBetweenPMinAndPMax(plant, tempResponse);
        }
    }

    private bool FindPowerBetweenPMinAndPMax(PowerplantModel plant, TemporaryResponseValues tempResponse)
    {
        tempResponse.TotalCapacity -= plant.PMin;

        // underload
        if (tempResponse.LoadGoal > tempResponse.TotalCapacity  + plant.PMax )
        {
            tempResponse.TotalCapacity += plant.PMax;
            tempResponse.TotalCost += plant.Cost * plant.PMin;
            tempResponse.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax ));
            return false;
        }

        // correct load
        else if (tempResponse.LoadGoal == tempResponse.TotalCapacity  + plant.PMax )
        {
            tempResponse.TotalCost += plant.Cost * plant.PMin;
            tempResponse.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax ));
            return true;
        }

        //overload
        else
        {
            while (tempResponse.TotalCapacity != tempResponse.LoadGoal)
            {
                plant.PMin += 0.1m ;
                tempResponse.TotalCapacity += plant.PMin ;
                if (tempResponse.TotalCapacity  != tempResponse.LoadGoal )
                    tempResponse.TotalCapacity -= plant.PMin;
                else
                {
                    tempResponse.TotalCost += plant.Cost * plant.PMin;
                    tempResponse.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMin));
                }
            };
            return false;
        }
    }

    private TemporaryResponseValues CompareCostWithoutWind(TemporaryResponseValues tempResult, List<PowerplantModel> sortedPowerplants)
    {
        var firstCost = tempResult.TotalCost;
        var sortedGasAndJetPowerplants = new List<PowerplantModel>();

        foreach (var plant in sortedPowerplants)
        {
            if (!plant.Type.Equals(ProjectConstants.Windturbine))
                sortedGasAndJetPowerplants.Add(plant);
        }
        var noWindTempResult = new TemporaryResponseValues(0.0m, tempResult.LoadGoal, 0.0m);
        foreach (var plant in sortedGasAndJetPowerplants)
        {
            ProcessGasAndJetFuelPlant(plant, noWindTempResult);
        }

        var secondCost = noWindTempResult.TotalCost;

        if (firstCost > secondCost)
            return noWindTempResult;
        else
            return tempResult;
    }

    private void CheckOverloadOrUnderload(TemporaryResponseValues tempResult, List<PowerplantModel> sortedPowerplants)
    {
        if (tempResult.PowerplantResponses.Sum(plant => plant.Power) != tempResult.LoadGoal)
        {
            if (tempResult.PowerplantResponses.Count == sortedPowerplants.Count || tempResult.PowerplantResponses.Count == 0)
                throw new Exception();
            else
                tempResult = ReProcessLoadBalancing(tempResult, sortedPowerplants);
        }
    }
}