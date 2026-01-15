db.createCollection('Events');
db.createCollection('Bookings');
db.createCollection('QrCodes');

// Test data
db.Events.insertMany([
    {
        title: "[AI Alliance] Trust and Safety Evaluations Initiative (TSEI)",
        hostedBy: "Data, Cloud and AI in Helsinki",
        timeStart: "2025-06-05T19:00:00Z",
        timeEnd: "2025-06-05T20:00:00Z"
    },
    {
        title: "Green Future Summit: Climate Tech Innovations",
        hostedBy: "Sustainable Berlin",
        timeStart: "2025-07-10T09:00:00Z",
        timeEnd: "2025-07-10T17:00:00Z"
    },
    {
        title: "NextGen Dev Conference 2025",
        hostedBy: "TechBuilders Community",
        timeStart: "2025-08-15T10:00:00Z",
        timeEnd: "2025-08-15T18:00:00Z"
    },
    {
        title: "Women in Data Science Meetup",
        hostedBy: "WiDS Amsterdam",
        timeStart: "2025-09-01T18:00:00Z",
        timeEnd: "2025-09-01T20:30:00Z"
    },
    {
        title: "Edge AI and IoT Integration Forum",
        hostedBy: "AI Nordic Collective",
        timeStart: "2025-10-12T13:00:00Z",
        timeEnd: "2025-10-12T16:00:00Z"
    },
    {
        title: "Startup Pitch Night: Smart Mobility",
        hostedBy: "Founders Garage Munich",
        timeStart: "2025-08-22T17:30:00Z",
        timeEnd: "2025-08-22T21:00:00Z"
    },
    {
        title: "NeuroTech Open Mic Night",
        hostedBy: "Neuroscience Berlin Hub",
        timeStart: "2025-07-20T18:00:00Z",
        timeEnd: "2025-07-20T20:00:00Z"
    },
    {
        title: "AI Ethics Hackathon",
        hostedBy: "FairAI Network",
        timeStart: "2025-11-03T09:00:00Z",
        timeEnd: "2025-11-03T19:00:00Z"
    },
    {
        title: "The Future of Robotics in Urban Spaces",
        hostedBy: "Smart Cities Europe",
        timeStart: "2025-10-18T11:00:00Z",
        timeEnd: "2025-10-18T14:30:00Z"
    },
    {
        title: "Remote Work Culture Camp",
        hostedBy: "PeopleOps Alliance",
        timeStart: "2025-09-25T15:00:00Z",
        timeEnd: "2025-09-25T18:00:00Z"
    }
]);

