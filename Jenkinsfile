pipeline {
    agent none
    environment {
        REGION = "ap-southeast-1"
        BACKEND_APP = "backend"
        ECR_URI = "008971674309.dkr.ecr.ap-southeast-1.amazonaws.com"
        IMAGE_TAG = "${BUILD_NUMBER}"
        GIT_REPO = "https://github.com/dungnh7/sd2376_msa.git"
        REPO_NAME = "sd2376_msa"
        EKS_NAME  = "sd2376eks"
    }
    stages {      
        stage('Docker Build and push Backend') {
            agent any
            steps {
                withAWS(region:"${REGION}",credentials:'aws-credential') {
                    sh "aws ecr get-login-password --region ${REGION} | docker login --username AWS --password-stdin ${ECR_URI}"
                    sh "docker build -t ${BACKEND_APP}:${IMAGE_TAG} src/"
                    sh "docker tag ${BACKEND_APP}:${IMAGE_TAG} ${ECR_URI}/${BACKEND_APP}:${IMAGE_TAG}"
                    sh "docker push ${ECR_URI}/${BACKEND_APP}:${IMAGE_TAG}"
                }
            }
        }

        stage('Update Kubernetes Deployment') {
            agent any
            steps {
                withAWS(region: "${REGION}", credentials: 'aws-credential') {
                    // Update the Kubernetes deployment file with the new image tag
                    sh """
                        sed -i 's|image: ${ECR_URI}/${BACKEND_APP}:.*|image: ${ECR_URI}/${BACKEND_APP}:${IMAGE_TAG}|' src/deployment/webapi-deployment.yaml
                        git config user.email "dung.nhd.7@gmail.com"
                        git config user.name "Jenkins"
                        git add src/deployment/webapi-deployment.yaml
                        git commit -m "Update backend image to ${IMAGE_TAG}" || echo "No changes to commit"
                        git push origin main || echo "No changes to push"
                    """
                }
            }
        }

        stage('One-time Setup Argocd') {
            agent any
            steps {
                withAWS(region: "${REGION}", credentials: 'aws-credential') {
                    script {
                        sh "aws eks update-kubeconfig --name ${EKS_NAME}"
                        // Check if ArgoCD namespace exists
                        def namespaceExists = (sh(script: "kubectl get namespace argocd", returnStatus: true) == 0)
                        if (!namespaceExists) {
                            sh "kubectl create namespace argocd"
                            sh "kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml"
                            // Wait for ArgoCD to be ready
                            //sh "kubectl wait --for=condition=available --timeout=30s deployment/argocd-server -n argocd"
                            // Retrieve and decode the Argo CD admin password
                            sh """
                                PASSWORD_BASE64=\$(kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath='{.data.password}')
                                echo "Decoding password..."
                                echo \${PASSWORD_BASE64} | base64 --decode
                            """
                            //forward port
                            sh "kubectl port-forward svc/argocd-server -n argocd 8082:443"
                            // Apply ArgoCD application
                            sh "kubectl create namespace app-argocd"
                            sh "kubectl apply -f declarative/backend.yaml"
                        } else {
                            echo "ArgoCD is already set up. Skipping one-time setup."
                        }
                    }
                }
            }
        }

        stage('One-time Setup Prometheus and Grafana') {
            agent any
            steps {
                withAWS(region: "${REGION}" , credentials: 'aws-credential') {
                    script {
                        sh "aws eks update-kubeconfig --name ${EKS_NAME}"
                        def namespaceExists = (sh(script: "kubectl get namespace monitoring", returnStatus: true) == 0)
                        if (!namespaceExists) {
                            // Check if Helm is installed and install it if not
                            if (sh(script: "which helm", returnStatus: true) != 0) {
                                sh 'curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash'
                                // Optionally add Helm to the PATH if not automatically available
                                sh 'export PATH=$PATH:/usr/local/bin'
                            }
                            // Install Prometheus
                            sh "helm repo add prometheus-community https://prometheus-community.github.io/helm-charts"
                            sh "helm repo update"
                            sh "helm install prometheus prometheus-community/kube-prometheus-stack --namespace monitoring --create-namespace"
                            // Install Grafana
                            sh "helm install grafana grafana/grafana --namespace monitoring --set adminPassword='admin' --set service.type=LoadBalancer"
    
                            //forward
                            sh "kubectl port-forward svc/prometheus-kube-prometheus-prometheus 9090:9090 -n monitoring"
                            sh "kubectl port-forward svc/grafana 3000:3000 -n monitoring"
                        } else {
                            echo "Prometheus and Grafana is already set up. Skipping one-time setup."
                        }      
                    }
                }
            }
        }
    }
}
