# Build static assets — run `npm install` locally first if you want reproducible builds via package-lock.json.
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json ./
RUN npm install

COPY . .
# Browser telemetry is compiled into the static bundle; these must be host-reachable URLs.
ARG VITE_OTEL_OTLP_HTTP_URL=http://localhost:4318
ARG VITE_OTEL_SAMPLE_RATIO=0.2
ENV VITE_OTEL_OTLP_HTTP_URL=$VITE_OTEL_OTLP_HTTP_URL \
    VITE_OTEL_SAMPLE_RATIO=$VITE_OTEL_SAMPLE_RATIO
RUN npm run build

FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
