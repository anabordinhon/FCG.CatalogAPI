$root = $PSScriptRoot

docker build -t catalogapi:latest --target runtime $root

docker build -t catalogapi-migrations:latest --target migrations $root

kubectl apply -f (Join-Path $root "infrastructure-sqlserver.yaml")
kubectl wait --for=condition=ready pod -l app=sqlserver --timeout=600s

kubectl apply -f (Join-Path $root "infrastructure-rabbitmq.yaml")
kubectl wait --for=condition=ready pod -l app=rabbitmq --timeout=300s

Start-Sleep -Seconds 10

kubectl apply -f (Join-Path $root "configmap.yaml")
kubectl apply -f (Join-Path $root "secret.yaml")
kubectl apply -f (Join-Path $root "service.yaml")

kubectl delete pod -l app=catalogapi --ignore-not-found
kubectl apply -f (Join-Path $root "deployment.yaml")

kubectl wait --for=condition=ready pod -l app=catalogapi --timeout=600s

kubectl get pods
kubectl get services

kubectl port-forward service/catalogapi-service 8080:80