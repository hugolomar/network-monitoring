pipeline {
    agent any

    environment {
        SONAR_HOST_URL = 'http://sonarqube:9000'
        SONAR_TOKEN = 'sqa_c71a2aeb53e618e0c6f4e246dd3e66ded09c6be0'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Backend - Prepare Analysis') {
            steps {
                echo 'Preparing SonarQube Analysis for .NET...'
                // Install sonarscanner if not present (simplified for this environment)
                sh 'dotnet tool install --global dotnet-sonarscanner --version 9.0.2 || true'
                sh 'export PATH="$PATH:$HOME/.dotnet/tools" && dotnet sonarscanner begin /k:"network-monitoring" /d:sonar.host.url="${SONAR_HOST_URL}" /d:sonar.token="${SONAR_TOKEN}" /d:sonar.cs.vscoveragexml.reportsPaths=coverage.xml'
            }
        }

        stage('Backend - Build & Test') {
            steps {
                echo 'Building and Testing Backend...'
                sh 'dotnet restore src/NetworkMonitoring.sln'
                sh 'dotnet build src/NetworkMonitoring.sln --no-restore -c Release'
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

        stage('SonarQube - End Analysis') {
            steps {
                echo 'Completing SonarQube Analysis...'
                sh 'export PATH="$PATH:$HOME/.dotnet/tools" && dotnet sonarscanner end /d:sonar.token="${SONAR_TOKEN}"'
            }
        }

        stage('Generate & Publish Docs') {
            steps {
                echo 'Generating Technical Wiki with DocFX...'
                sh 'dotnet tool restore'
                sh 'dotnet docfx metadata'
                sh 'dotnet docfx build'
                echo 'Publishing to documentation server...'
                // Copy the generated site to the shared volume defined in docker-compose
                sh 'cp -R _site/* /var/jenkins_home/docs-site/'
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
