# FruitsBackend

Backend Project to Integrate with extenal API provided by fruityvice and we are filttering the Healthiest fruits

API URL: https://www.fruityvice.com/api/

## Prerequirements

* Visual Studio 2022 OR VS Code
* .NET Core SDK 9

## How To Run

* Open Terminal and Change the directory to the project
* To Run the server/application please use the following command:
    * dotnet run
    * and you can test it using this URL: http://localhost:5111/api/fruits/healthiest?minSugar=10&maxSugar=30
* To Run the unit tests please use the following command
    * dotnet test

## NOTE About This Task

I was thinking about load all the fruits during project startup using a singleton service that will have a data structure like HashSet that will contains all the fruits, and when the user hit our API, we will just filter the fruits, this would make our API very fast since we are not calling any external API inside the controller, but I can see there API to add fruits which is 'https://www.fruityvice.com/doc/index.html#api-PUT-AddFruit', so the db is not having a constant data, so we can't do this singleton service, so I put all the logic when the user call the controller action.
