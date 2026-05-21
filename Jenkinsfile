pipeline {
    agent any

    environment {
        SONAR_HOST_URL = 'http://sonarqube:9000'
        // For demonstration purposes, we assume tools are available in the environment
        // In a real Jenkins, these would be configured via 'tools' or Docker agents
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Backend - Restore') {
            steps {
                echo 'Restoring .NET dependencies...'
                sh 'dotnet restore src/NetworkMonitoring.sln'
            }
        }

        stage('Backend - Build') {
            steps {
                echo 'Building Backend...'
                sh 'dotnet build src/NetworkMonitoring.sln --no-restore -c Release'
            }
        }

        stage('Backend - Test') {
            steps {
                echo 'Running Backend Tests...'
                sh 'dotnet test src/NetworkMonitoring.sln --no-build -c Release'
            }
        }

        stage('Frontend - Lint & Build') {
            steps {
                dir('src/NetworkMonitoring.Frontend') {
                    echo 'Linting and Building Frontend...'
                    sh 'npm install'
                    sh 'npm run lint'
                    sh 'npm run build'
                }
            }
        }

        stage('SonarQube Analysis') {
            steps {
                echo 'Simulating SonarQube scan...'
                // In a configured environment, we would use the SonarScanner here:
                // sh 'dotnet sonarscanner begin /k:"network-monitoring" /d:sonar.host.url="${SONAR_HOST_URL}"'
                // sh 'dotnet build src/NetworkMonitoring.sln'
                // sh 'dotnet sonarscanner end'
                echo 'Analysis report would be sent to http://localhost:9000'
            }
        }
    }

    post {
        always {
            echo 'Pipeline finished.'
        }
        failure {
            echo 'Pipeline failed. Check the logs for documentation or build errors.'
        }
    }
}
