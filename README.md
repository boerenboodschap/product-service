# product-service

[![GitHub Super-Linter](https://github.com/boerenboodschap/product-service/actions/workflows/CI-CD.yml/badge.svg)](https://github.com/marketplace/actions/super-linter)

## Run the application

### Locally

with docker-compose: `docker compose up`

with kubernetes:

1. `helm install mongodb oci://registry-1.docker.io/bitnamicharts/mongodb`

2. Zoek in kubernetes secrets naar de credentials van de database en zet die in de connectionstring in deployment.yaml.

3. `helm install product-service ./helm`

## status

This dotnet 7.0 api can handle basic CRUD operations on a mongoDB database that can be run with docker-compose.

Start the dev server with: `cd ./src/docker-compose && docker compose up`
