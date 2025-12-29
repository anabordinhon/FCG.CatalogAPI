docker build -t catalogapi:latest .

kubectl apply -f k8s/sqlserver-deployment.yaml
kubectl wait --for=condition=ready pod -l app=sqlserver --timeout=180s

kubectl apply -f k8s/rabbitmq-deployment.yaml
kubectl wait --for=condition=ready pod -l app=rabbitmq --timeout=120s

Start-Sleep -Seconds 10

kubectl apply -f k8s/catalogapi-configmap.yaml
kubectl apply -f k8s/catalogapi-secret.yaml
kubectl apply -f k8s/catalogapi-service.yaml
kubectl apply -f k8s/catalogapi-deployment.yaml

kubectl wait --for=condition=ready pod -l app=catalogapi --timeout=180s

kubectl get pods
kubectl get services