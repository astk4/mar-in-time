FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./

RUN npm install

COPY . .

RUN ./fetch_frontend_libs.sh && npm run build --prod

FROM nginx:alpine

COPY /nginx/container.conf /etc/nginx/conf.d/container.conf

COPY --from=build /app/dist/mar-in-time-frontend/browser /usr/share/nginx/html

RUN cp "/usr/share/nginx/html/assets/container_config.json" "/usr/share/nginx/html/assets/config.json" \
    && mv "/etc/nginx/conf.d/container.conf" "/etc/nginx/conf.d/default.conf" \
    && cp "/usr/share/nginx/html/favicon_container.ico" "/usr/share/nginx/html/favicon.ico"

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]