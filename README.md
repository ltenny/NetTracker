# NetTracker
This app uses PcapDotNet and MaxMind GeoIP to track IP destination addresses for all network communications from
a specific NIC. This app requires Pcap.  The best way to get this is to install Wireshark (which installs Pcap). 
If you are interested in this app, you'll also be interested in Wireshark.

Summary
-------

Inspired by the Ticketmaster data breach in June 2018, this simple Windows command line application follows all of 
the hosts with which a machine communicates.  The core vulnerability exploited in the Ticketmaster data breach is the unrestricted
execution of javascript in a browser.  Several of the third party javascript apps used by Ticketmaster were infected
with javascript keylogging code.  The code sent sensitive information to a command-and-control server in eastern Europe. 
A simple way to detect this kind of attack is to exercise the website on a real machine using a real browser and track all 
of the out-bound network traffic.  NetTracker tracks this outbound traffic by host.

tl;dr
-----

The indication of compromise we focus on here is outbound traffic to unexpected hosts primarily distinquished by country.

Use
---
- install Wireshark, this app used Pcap which is installed by the Wireshark installer
- get the free GeoLite2 country database from https://dev.maxmind.com/geoip/geoip2/geolite2/
- copy the database to either the current directory or in the root of the C:\
- run NetTracker
- unique IPs outside the USA are printed to the console. Suspect countries are printed in red. 
  All unique IPs are logged in files in the current directory
- Tested under the latest version of Windows 10, you mileage will likely vary with other versions
  
