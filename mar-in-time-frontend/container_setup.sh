cp /usr/share/nginx/html/assets/container_config.json "/usr/share/nginx/html/assets/config.json"
mv /etc/nginx/conf.d/container.conf /etc/nginx/conf.d/default.conf

nginx -g 'daemon off;'