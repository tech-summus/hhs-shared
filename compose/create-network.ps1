docker network create -d bridge hhs.infrastructure-network

docker-compose -f compose/infrastructure.yml -p hhs-infra up -d
docker-compose -f -p hhs-infra logs --follow
docker-compose -f compose/infrastructure.yml -p hhs-infra down
