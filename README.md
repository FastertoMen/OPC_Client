# UVP280_Client
This software gives an ability to connect to UVP280-device through EKON134 or MOXA and download a report from it. 
UVP280 and EKON134/MOXA communicate via serial port. EKON134/MOXA and UVP280_Client communicate via ethernet. 
It's required to install NPortAdministrator for MOXA, and VirtualPortConfigurator for EKON134 (supported up to WIN7) due to create virtual srial ports for them.
Software send a request to specific Modbus registers on UVP280 and recieve a report as an answer. Results of a request stores in a SQLite database.
Software communicate with 5 separate devices. 
In options user can specify serial ports, time of request, delay time and devices which will be interrogated.
