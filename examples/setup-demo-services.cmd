:: Create the needed volumns for the services used by the Wiki examples:
docker volume create --name=dev-rabbit_data
docker volume create --name=dev-rabbit_logs
docker volume create --name=dev-mongo_data
docker volume create --name=dev-mongo_logs
docker volume create --name=dev-seq_data

:: Run all the needed services within Docker
docker-compose up -d

