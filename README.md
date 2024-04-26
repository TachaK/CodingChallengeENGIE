# EnergyPracticedotnet run --urls=http://localhost:8888

The API will be accessible at http://localhost:8888.
### 2. HTTPS
To run the API in HTTPS, use the following command:

dotnet run --urls=https://localhost:8888

### 3. DockerTo run the API using Docker, build the Docker image and run a container with the following commands:

docker build -t powerplant-api .docker run -p 8888:8888 -t powerplant-api

### API Endpoint
Once the API is running, you can access the production plan calculation endpoint:
POST http://localhost:8888/productionplan
