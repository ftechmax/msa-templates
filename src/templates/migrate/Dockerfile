FROM node:19-alpine as build
RUN mkdir -p /app
WORKDIR /app
COPY . /app
RUN npm ci

FROM node:19-alpine
WORKDIR /app
COPY --from=build --chown=node:node /app/db.js /app/migrate.js /app/mongodb-store.js /app/*.json /app/
COPY --from=build --chown=node:node /app/migrations /app/migrations/
COPY --from=build --chown=node:node /app/node_modules /app/node_modules/
USER node
CMD ["npm", "run", "migrate"]
