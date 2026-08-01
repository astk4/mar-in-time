FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./

RUN npm install

COPY . .

RUN ./fetch_frontend_libs.sh && npm run build --prod

FROM nginx:alpine

COPY /nginx/container.conf /etc/nginx/conf.d/container.conf

COPY --from=build /app/dist/mar-in-time-frontend/browser /usr/share/nginx/html

EXPOSE 80