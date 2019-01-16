# docker image build -t boondocks-dashboard . -f Dashboard.Dockerfile 
# docker run -p 3000:80 --rm boondocks-dashboard

FROM node:9.6.1 AS build-spa

RUN mkdir /usr/src/boondocks-dashboard
WORKDIR /usr/src

ENV PATH /usr/src/boondocks-dashboard/node_modules/.bin:$PATH

# Development Configuration Files:
COPY ./package.json boondocks-dashboard/package.json
COPY ./angular.json boondocks-dashboard/angular.json
COPY ./tsconfig.json boondocks-dashboard/tsconfig.json
COPY ./tslint.json boondocks-dashboard/tslint.json

# Unit-Test Tools and Application Source:
COPY ./e2e boondocks-dashboard/e2e
COPY ./src boondocks-dashboard/src

WORKDIR /usr/src/boondocks-dashboard

# Install the Node Packages:
RUN npm install
RUN npm install -g @angular/cli

# Build Application
RUN ng build --prod --output-path /out/

FROM nginx:alpine

COPY ./nginx.conf /etc/nginx/nginx.conf

WORKDIR /usr/share/nginx/html
COPY --from=build-spa /out/ .
