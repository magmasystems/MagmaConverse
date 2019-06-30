# MagmaConverse

![MagmaConverse Architecture](/MagmaConverse%20Architecture.jpg)

----

* JSON-based form definition language developed
    * Client “interviews” are driven through forms
    * You can load and retrieve form definitions, create an instance of a form, and save a form

* Console-based interaction
    * Eventual support for Web and WPF

* Everything can be driven through REST
    * Postman can be used to drive a demo using automated input

* Support for MongoDB for persistence
    * The persistence driver layer can support other persistence mechanisms

* Kafka Messaging
  
----  

* JSON-based Form Definition
    * Defines multiple forms, reference data, etc
    * Fields have
        * Validation Functions
        * Actions
        * Submission functions (for buttons)

    * JavaScript expressions supported through Jint

    * Workflow supported
