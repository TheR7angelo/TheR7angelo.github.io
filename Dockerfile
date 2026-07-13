FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y python3 && rm -rf /var/lib/apt/lists/*

COPY . .

RUN dotnet workload install wasm-tools

RUN dotnet restore

RUN dotnet publish "TheR7angelo.github.io/TheR7angelo.github.io.csproj" -c Release -o /app/publish

FROM nginx:alpine
WORKDIR /usr/share/nginx/html

COPY --from=build /app/publish/wwwroot .

RUN printf 'server {\n\
    listen 80;\n\
    location / {\n\
        root /usr/share/nginx/html;\n\
        try_files $uri $uri/ /index.html =404;\n\
    }\n\
}\n' > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]