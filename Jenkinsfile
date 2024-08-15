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
                        git init
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
                        def namespaceExists = (sh(script: "kubectl get namespace nameargocd", returnStatus: true) == 0)
                        if (!namespaceExists) {
                            sh "kubectl create namespace nameargocd"
                            sh "kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml"
                            // Wait for ArgoCD to be ready
                            sh "kubectl wait --for=condition=available --timeout=300s deployment/argocd-server -n nameargocd"
                            // Retrieve and decode the Argo CD admin password
                            sh """
                                PASSWORD_BASE64=\$(kubectl -n nameargocd get secret argocd-initial-admin-secret -o jsonpath='{.data.password}')
                                echo "Decoding password..."
                                echo \${PASSWORD_BASE64} | base64 --decode
                            """
                            //forward port
                            sh "kubectl port-forward svc/argocd-server -n nameargocd 8081:443 &"
                            // Apply ArgoCD application
                            def namespaceAppExists = (sh(script: "kubectl get namespace app-nameargocd", returnStatus: true) == 0)
                            if (!namespaceAppExists) {
                                sh "kubectl create namespace app-nameargocd"
                            }
                            sh "kubectl apply -f declarative/project.yaml"
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
                                //sh "sudo curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash"
                                def installDir = '/var/lib/jenkins/bin'
                                sh "mkdir -p ${installDir}"                                
                                // Download and run the Helm installation script
                                sh """
                                    curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3
                                    chmod 700 get_helm.sh
                                    HELM_INSTALL_DIR=${installDir} ./get_helm.sh --no-sudo
                                    rm get_helm.sh
                                """                                
                                // Add the installation directory to PATH
                                env.PATH = "${installDir}:${env.PATH}"                
                            }
                            // Install Prometheus
                            sh "helm repo add prometheus-community https://prometheus-community.github.io/helm-charts"
                            sh "helm repo add stable https://charts.helm.sh/stable"
                            sh "helm repo update"
                            sh "helm install prometheus prometheus-community/kube-prometheus-stack --namespace monitoring --create-namespace"
                            sh "kubectl port-forward service/prometheus-grafana 8082:80 &"
                            // // Install Grafana
                            // sh "helm repo add grafana https://grafana.github.io/helm-charts"
                            // sh "helm repo update"
                            // sh "helm install grafana grafana/grafana --namespace monitoring --set adminPassword='admin' --set service.type=LoadBalancer"
    
                            // //forward
                            // sh "kubectl port-forward svc/prometheus-kube-prometheus-prometheus 9090:9090 -n monitoring &"
                            // sh "kubectl port-forward svc/grafana 3000:3000 -n monitoring &"
                        } else {
                            echo "Prometheus and Grafana is already set up. Skipping one-time setup."
                        }      
                    }
                }
            }
        }
    }
}
