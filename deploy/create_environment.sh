# this script will create an environment on target Kubernetes cluster
k8sProvider=$1 #gke or dd (docker-desktop)

kubectl apply -f ./api/mightycalc-api-deploy.yaml
kubectl apply -f ./api/mightycalc-api-svc.yaml
kubectl apply -f ./api/mightycalc-api-svc-external-${k8sProvider}.yaml

kubectl apply -f ./persistence/persistence-deploy.yaml
kubectl apply -f ./persistence/persistence-svc.yaml
if("$k8sProvider" -eq "dd")
then
kubectl apply -f ./persistence/persistence-svc-external.yaml
fi
kubectl apply -f ./persistence/storageclass-${k8sProvider}.yaml
