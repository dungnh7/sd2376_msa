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
                    sh "kubectl apply -f src/deployment/webapi-deployment.yaml"
                    sh "kubectl apply -f src/deployment/webapi-service.yaml"
                }
            }
        }
    }
}
