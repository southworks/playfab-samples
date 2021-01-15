# Fantasy Soccer - SOUTHWORKS

 ![presentation][presentation-img]

## Summary

This workshop consists on a web fantasy soccer application using PlayFab service to manage game data.

Users will be able to register with a PlayFab account, or a Microsoft Account on an specific Azure Active Directory.
Once registered, they will have an limited budget of a virtual currency (*FS*) to make offers on soccer players on the Marketplace section.

When a soccer player joins to the user's team, starts as a benched player. Then, the user can go to the Team Management section, and add them to the starters team.

This application doesn't consume any data of the real world, so we have implemented a tool to mock all the data needed into the databases.
Also, we've included a way to simulate the rounds of a tournament, so you can se how scores are processed and listed in PlayFab Leaderboards.

Finally, we configured Azure Pipelines for provisioning and continuous deployment.

## Highlights

If you want to run this application or understand how it works, you can read the following topics.

- [Fantasy Soccer web architecture document][doc-web-architecture].
- [Data Seeder document][doc-data-seeder].
- [Simulation document][doc-simulator].
- [Azure Pipelines implementation document][doc-pipelines].
- [Configuration and starting up][doc-configuration-and-starting-up]

<!-- Images -->
[presentation-img]: ./documentation-assets/presentation.png

<!-- Document -->
[doc-data-seeder]: ./dataseeder.md
[doc-web-architecture]: ./web-architecture.md
[doc-pipelines]: ./pipelines.md
[doc-simulator]: ./simulation.md
[doc-configuration-and-starting-up]: ./configuration-and-starting-up.md
