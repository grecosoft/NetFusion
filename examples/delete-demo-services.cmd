docker-compose down

docker volume rm dev-rabbit_data
docker volume rm dev-rabbit_logs
docker volume rm dev-mongo_data
docker volume rm dev-mongo_logs
docker volume rm dev-seq_data