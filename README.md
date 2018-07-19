# NetTracker
Uses PcapDotNet and MaxMind GeoIP to track the hosts their countrys a machine communicates with.

Inspired by the Ticketmaster data breach in June 2018, this simple app follows all of the hosts with which
a machine communicates.  The core vulnerability exploited in the Ticketmaster data breach is the unrestricted
execution of javascript in a browser.  Several of the third party javascript apps used by Ticketmaster were infected
with javascript keylogging code.  The code sent sensitive information to a command-and-control server in eastern Europe. 
A simple way to detect this kind of attack is to exercise the website on a real machine using a real browser and track all 
of the out-bound network traffic.  NetTracker tracks this outbound traffic by host.


Use
---

- get the free GeoLite2 country database from https://dev.maxmind.com/geoip/geoip2/geolite2/
- copy the database to either the current directory or in the root of the C:\
- run NetTracker
- unique IPs outside the USA are printed to the console. All unique IPs are logged in files in the
  current directory
  
