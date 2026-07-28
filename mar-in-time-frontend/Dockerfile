FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./

RUN npm install

COPY . .

RUN npm run build --prod


FROM nginx:alpine

COPY container_setup.sh ./

RUN sed -i 's/\r$//' container_setup.sh

COPY /nginx/container.conf /etc/nginx/conf.d/container.conf

COPY --from=build /app/dist/mar-in-time-frontend/browser /usr/share/nginx/html

EXPOSE 80

CMD /container_setup.sh