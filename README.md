# sd2376_msa
- Using the webhook
- Declarative
    - Create argocd project
    - Create argocd applications
- Jenkins:
    - Docker Build and push to ECR
    - Update Kubernetes Deployment // Update the Kubernetes deployment file with the new image tag
    - One time step install argocd
    - Deploy argo declarative file: project and applications
    - One time install Prometheus and Grafana
