## Mighty Calculator 

MightyCalc is a demo project to show usage of Akka cluster and DDD/ES/CQRS design in the real world. 

MightyCalc is a calculator app with REST API. 
It provides basic functionality like: 

*  Perform calculations (add, subtract, multiply, divide, square root, cube root, power, and factorial).
*  A report showing the operations that have been done during the given period
*  Allow user to add its own functions

## Usage

  MightyCalc exposes SwaggerUI for human interaction. 
  Check [online example](http://mightycalc.space/index.html) or run MC locally and go to http://localhost:30010. 

## Deployment 
   
   MightyCalc is designed for Kubernetes deployment but doesn't depend on k8s-specific features. So it could be deployed by any other tools supporting Docker containers. 
   To deploy MC to k8s use [environment creation script](https://github.com/andreyleskov/MightyCalc/blob/master/deploy/create_environment.sh). It supports several k8s providers, like Google Kubernetes Engine, Docker for Desktop, and Minikube.  

   Each component is shipped as Docker image, you can find it on [docker hub](https://hub.docker.com/u/aleskov). Images are built by [CI\CD pipeline](https://ci.appveyor.com/project/ContextCore/mightycalc)  

## Components 

   MightyCalc consists of four main components: 

*  [**API 
  nodes**](https://github.com/andreyleskov/MightyCalc/tree/master/src/MightyCalc.API) - expose REST API and delegate all the work to worker nodes. Uses the database to get reports data. Exposes SwaggerUI generated by [API reference](https://github.com/andreyleskov/MightyCalc/blob/master/src/MightyCalcAPI.yaml)
Implemented with Asp.Net Core and Akka.Net.
*  [**Worker nodes**](https://github.com/andreyleskov/MightyCalc/tree/master/src/MightyCalc.NodeHost) - perform the calculation requested by API, implement the business logic in CQRS/ES/DDD manner.
*  [**Database**](https://github.com/andreyleskov/MightyCalc/blob/master/build/persistence/persistence.dockerfile) - PosgreSQL database instance. Hosts three databases: 
*journal*,
*readmodel*,
*snapshotstore*
*  [**Seed nodes**](https://github.com/andreyleskov/MightyCalc/tree/master/src/MightyCalc.LightHouse) - used to form Akka cluster and connect API nodes to Worker nodes. 
* [**C# API Client**](https://github.com/andreyleskov/MightyCalc/tree/master/src/MightyCalc.Client) - provides a library for REST API access, autogenerated from [API reference](https://github.com/andreyleskov/MightyCalc/blob/master/src/MightyCalcAPI.yaml). 

## Maintainance 

Api, worker and seed nodes host [Petabridge.Cmd](https://cmd.petabridge.com/), and it can be used to get various useful information and perform operations. Like get the current cluster status. 

Usage example, from the k8s host: 

```
kubectl exec -it seed-0 -- /bin/bash        
```

```
pbm localhost:9110  
```

## Build
 MightyCalc is supposed to run build as a part of Docker image creation. A single Dockerfile will can produce several components: [api](https://github.com/andreyleskov/MightyCalc/blob/master/src/Dockerfile#L58), [seed](https://github.com/andreyleskov/MightyCalc/blob/master/src/Dockerfile#L80) and [worker](https://github.com/andreyleskov/MightyCalc/blob/master/src/Dockerfile#L69) nodes, [test pod](https://github.com/andreyleskov/MightyCalc/blob/master/src/Dockerfile#L34).
See build command examples at [CI/CD pipeline](https://github.com/andreyleskov/MightyCalc/blob/master/appveyor.yml#L50).

API is determined by OpenAPI specs, swaggerUI and the client will be [generated](https://github.com/andreyleskov/MightyCalc/blob/master/src/Dockerfile#L28) out of it during the build.
