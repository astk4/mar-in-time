cp /usr/share/nginx/html/assets/container_config.json "/usr/share/nginx/html/assets/config.json"
mv /etc/nginx/conf.d/container.conf /etc/nginx/conf.d/default.conf

cp /usr/share/nginx/html/favicon_container.ico /usr/share/nginx/html/favicon.ico

nginx -g 'daemon off;'