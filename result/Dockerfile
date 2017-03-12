FROM node:5.11.0-slim

WORKDIR /app

RUN npm install -g nodemon
ADD . /app
RUN npm install 

ENV PORT 80
ENV AZURE_STORAGE_ACCOUNT account
ENV AZURE_STORAGE_ACCESS_KEY key

EXPOSE 80

CMD ["node", "server.js"]
