pipeline {
    agent none
    environment {
        REGION = "ap-southeast-1"
        BACKEND_APP = "backend"
        ECR_URI = "008971674309.dkr.ecr.ap-southeast-1.amazonaws.com"
        IMAGE_TAG = "${BUILD_NUMBER}"
        GIT_REPO = "https://github.com/dungnh7/sd2376_msa.git"
        REPO_NAME = "sd2376_msa"
    }
    stages {        
        stage('Docker Build Backend') {
            agent any
            steps {
                withAWS(region:'ap-southeast-1',credentials:'aws-credential') {
                    sh "aws ecr get-login-password --region ${REGION} | docker login --username AWS --password-stdin ${ECR_URI}"
                    sh "docker build -t ${BACKEND_APP}:${IMAGE_TAG} src/"
                    sh "docker tag ${BACKEND_APP}:${IMAGE_TAG} ${ECR_URI}/${BACKEND_APP}:latest"
                    sh "docker push ${ECR_URI}/${BACKEND_APP}:latest"
                }
            }
        }

        stage('Deploy k8s') {
            agent any
            steps {
                withAWS(region: 'ap-southeast-1', credentials: 'aws-credential') {
                    sh "aws eks update-kubeconfig --name sd2376eks"
                    // Create namespace
                    sh "kubectl create namespace argocd || echo argocdnamespacealreadyexists"
                    sh "kubectl create namespace app-argocd || echo appargocdnamespacealreadyexists"
                    //install argocd
                    sh "kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml"
                    // Retrieve and decode the Argo CD admin password
                    sh """
                        PASSWORD_BASE64=$(kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath='{.data.password}')
                        echo "Decoding password..."
                        echo \${PASSWORD_BASE64} | base64 --decode
                       """
                    //forward port
                    sh "kubectl port-forward svc/argocd-server -n argocd 8082:443"
                    //deploy services
                    sh "kubectl apply -f src/deployment/webapi-deployment.yaml"
                    sh "kubectl apply -f src/deployment/webapi-service.yaml"
                    //run deploy argocd declarative
                    sh "kubectl apply -f declarative/backend.yaml"

                    // Install Prometheus
                    sh "helm repo add prometheus-community https://prometheus-community.github.io/helm-charts"
                    sh "helm repo update"
                    sh "helm install prometheus prometheus-community/kube-prometheus-stack --namespace monitoring --create-namespace"
                    // Install Grafana
                    sh "helm install grafana grafana/grafana --namespace monitoring --set adminPassword='admin' --set service.type=LoadBalancer"

                    //forward
                    sh "kubectl port-forward svc/prometheus-kube-prometheus-prometheus 9090:9090 -n monitoring"
                    sh "kubectl port-forward svc/grafana 3000:3000 -n monitoring"
                }
            }
        }
    }
}
