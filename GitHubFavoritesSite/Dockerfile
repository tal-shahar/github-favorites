FROM node:20-alpine AS build
WORKDIR /app
COPY GitHubFavoritesSite/package*.json ./
RUN npm install
COPY GitHubFavoritesSite/ .
RUN npm run build

FROM nginx:1.27-alpine
COPY GitHubFavoritesSite/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/git-hub-favorites-site/browser /usr/share/nginx/html

