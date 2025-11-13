FROM node:lts-alpine as build
ENV NG_BUILD_CACHE=0
ENV NG_PERSISTENT_BUILD_CACHE=0
ENV NODE_OPTIONS=--max_old_space_size=2048

RUN mkdir -p /app
WORKDIR /app
COPY . /app

RUN npm ci
RUN npm run build --configuration="production" --aot

FROM nginx:1.28.0-alpine3.21
RUN chown -R nginx:nginx /var/cache/nginx && \
    chown -R nginx:nginx /var/log/nginx && \
    chown -R nginx:nginx /etc/nginx/conf.d && \
    touch /var/run/nginx.pid && \
    chown nginx:nginx /var/run/nginx.pid
COPY nginx.conf /etc/nginx/nginx.conf
COPY --chown=nginx:nginx default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build --chown=nginx:nginx /app/dist /usr/share/nginx/html
USER nginx
