using Core.Constants;
using Core.Dto;
using Core.Enum;
using Core.Exceptions;
using Core.Interface;
using Core.Mapper;
using Core.Models;
using Mapster;

namespace Application;

public class ComputeSolution : IComputeSolution
{
    public Response Solution(Payload payload)
    {
        var payloadDto = payload.Adapt<PayloadDto>();
        var tempResult = new TemporaryResponseValues(payloadDto.Load);
        var sortedPowerplants = payloadDto.GetPowerplantMeritOrder().ToList();

        foreach (var plant in sortedPowerplants)
        {
            // Wind powerplants
            if (PlantTypeEnum.windturbine.Equals(plant.Type)
                && plant.PMax != 0)
            {
                if (IsLoadReachedWithWindTurbinePlant(plant, tempResult))
                {
                    break;
                }
            }

            // Non-wind powerplants
            else if (!PlantTypeEnum.windturbine.Equals(plant.Type)
                    && plant.PMax != 0)
            {
                if (IsLoadReachedWithGasAndJetFuelPlant(plant, tempResult))
                {
                    break;
                }
            }
        }

        // Method checks if the load is reachable; throws exception if overloads or underloads
        CheckOverloadOrUnderload(tempResult, sortedPowerplants);

        // Once a solution is found, the method checks if there is a more affordable one without considering wind powerplants
        tempResult = CompareCostWithoutWind(tempResult, sortedPowerplants);

        // Map to response type
        var result = new Response();
        result.PowerplantResponses.AddRange(tempResult.PowerplantResponses);

        foreach (var powerPlant in sortedPowerplants)
        {
            if (!result.PowerplantResponses.Any(p => p.Name == powerPlant.Name))
                result.PowerplantResponses.Add(new PowerplantResponse(powerPlant.Name, 0.0m));
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

        mostUsedPowerplant.P -= unusedPowerplant.PMin;
        tempResult.TotalCapacity -= (unusedPowerplant.PMin + mostExpensivePowerplant.P);
        tempResult.PowerplantResponses.Remove(mostExpensivePowerplant);

        var powerStillNeeded = tempResult.LoadGoal - tempResult.TotalCapacity;

        var powerPlantToAdd = new PowerplantResponse(unusedPowerplant.Name, unusedPowerplant.PMin);

        tempResult.TotalCapacity += unusedPowerplant.PMin;
        mostUsedPowerplant.P += (powerStillNeeded - unusedPowerplant.PMin);
        tempResult.TotalCapacity += (powerStillNeeded - unusedPowerplant.PMin);

        tempResult.PowerplantResponses.Add(powerPlantToAdd);

        return tempResult;
    }

    private static PowerplantResponse GetMostPoweredPlant(TemporaryResponseValues response)
    {
        var maxPower = response.PowerplantResponses.Max(p => p.P);
        return response.PowerplantResponses.First(item => item.P == maxPower);
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
            if (plant.PMax == 0)
            {
                continue;
            }
            if (response.PowerplantResponses.All(p => p.Name != plant.Name))
            {
                 return plant;
            }
        }
        return null;
    }

    private bool IsLoadReachedWithWindTurbinePlant(PowerplantModel plant, TemporaryResponseValues tempResponse)
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
            if (tempResponse.TotalCapacity > tempResponse.LoadGoal)
            {
                tempResponse.TotalCapacity -= plant.PMax;
                return false;
            }

            // correct load
            tempResponse.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax));
            return true;

        }
    }

    private bool IsLoadReachedWithGasAndJetFuelPlant(PowerplantModel plant, TemporaryResponseValues tempResponse)
    {

        tempResponse.TotalCapacity += plant.PMin;

        // correct load
        if (tempResponse.TotalCapacity == tempResponse.LoadGoal && plant.PMin != 0)
        {
            tempResponse.TotalCost += plant.Cost * plant.PMin;
            tempResponse.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMin));
            return true;
        }

        // overload
        if (tempResponse.TotalCapacity > tempResponse.LoadGoal)
        {
            tempResponse.TotalCapacity -= plant.PMin;
            return false;
        }

        // underload
        // If PMin added is less than load, check if PMax can be added. If not, find the power between PMin and PMax for this plant
        return FindPowerBetweenPMinAndPMax(plant, tempResponse);

    }

    private bool FindPowerBetweenPMinAndPMax(PowerplantModel plant, TemporaryResponseValues tempResponse)
    {
        tempResponse.TotalCapacity -= plant.PMin;

        // underload
        if (tempResponse.LoadGoal > tempResponse.TotalCapacity + plant.PMax)
        {
            tempResponse.TotalCapacity += plant.PMax;
            tempResponse.TotalCost += plant.Cost * plant.PMin;
            tempResponse.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax));
            return false;
        }

        // correct load
        if (tempResponse.LoadGoal == tempResponse.TotalCapacity + plant.PMax)
        {
            tempResponse.TotalCost += plant.Cost * plant.PMin;
            tempResponse.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMax));
            return true;
        }

        //overload
        while (tempResponse.TotalCapacity != tempResponse.LoadGoal)
        {
            plant.PMin += 0.1m;
            tempResponse.TotalCapacity += plant.PMin;
            if (tempResponse.TotalCapacity != tempResponse.LoadGoal)
            {
                tempResponse.TotalCapacity -= plant.PMin;
            }
            else
            {
                tempResponse.TotalCost += plant.Cost * plant.PMin;
                tempResponse.PowerplantResponses.Add(new PowerplantResponse(plant.Name, plant.PMin));
            }
        };
        return false;
    }

    private TemporaryResponseValues CompareCostWithoutWind(TemporaryResponseValues tempResult, List<PowerplantModel> sortedPowerplants)
    {
        var firstCost = tempResult.TotalCost;
        var sortedGasAndJetPowerplants = new List<PowerplantModel>();

        foreach (var plant in sortedPowerplants)
        {
            if (!plant.Type.Equals(PlantTypeEnum.windturbine))
            {
                sortedGasAndJetPowerplants.Add(plant);
            }
        }
        var noWindTempResult = new TemporaryResponseValues(tempResult.LoadGoal);
        foreach (var plant in sortedGasAndJetPowerplants)
        {
            IsLoadReachedWithGasAndJetFuelPlant(plant, noWindTempResult);
        }

        var secondCost = noWindTempResult.TotalCost;

        if (firstCost > secondCost)
        {
            return noWindTempResult;
        }

        return tempResult;
    }

    private void CheckOverloadOrUnderload(TemporaryResponseValues tempResult, List<PowerplantModel> sortedPowerplants)
    {
        if (tempResult.PowerplantResponses.Sum(plant => plant.P) != tempResult.LoadGoal)
        {
            if (tempResult.PowerplantResponses.Count == sortedPowerplants.Count || tempResult.PowerplantResponses.Count == 0)
            {
                if (tempResult.PowerplantResponses.Sum(plant => plant.P) < tempResult.LoadGoal)
                {
                    throw new CustomException("The load is unreachable");
                }
                else
                {
                    throw new CustomException("The load is exceeded");
                }
            }

            tempResult = ReProcessLoadBalancing(tempResult, sortedPowerplants);
        }
    }
}