db.createCollection('WeatherForcast');
db.WeatherForecasts.insertMany([
    {
        Date: "2025-07-07T00:00:00Z",
        TemperatureC: 19,
        Summary: "Cool"
    },
    {
        Date: "2025-07-08T00:00:00Z",
        TemperatureC: 11,
        Summary: "Freezing"
    },
    {
        Date: "2025-07-09T00:00:00Z",
        TemperatureC: 25,
        Summary: "Hot"
    },
    {
        Date: "2025-07-10T00:00:00Z",
        TemperatureC: 23,
        Summary: "Cool"
    },
    {
        Date: "2025-07-11T00:00:00Z",
        TemperatureC: 50,
        Summary: "Cool"
    }
]);
