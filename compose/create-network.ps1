docker network create -d bridge infrastructure-network

docker-compose -f compose/infrastructure.yml -p infra up -d
docker-compose -f -p infra logs --follow
docker-compose -f compose/infrastructure.yml -p infra down
