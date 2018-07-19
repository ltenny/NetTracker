# NetTracker
Uses PcapDotNet to track the hosts a machine communicates with.

Inspired by the Ticketmaster data breach in June 2018, this simple app follows all of the hosts with which
a machine communicates.  The core vulnerability exploited in the Ticketmaster data breach is the unrestricted
execution of javascript in a browser.  Several of the third party javascript apps used by Ticketmaster were infected
with javascript keylogging code.  The code sent sensitive information to a command-and-control server in eastern Europe. 
A simple way to detect this kind of attack is to exercise the website on a real machine using a real browser and track all 
of the out-bound network traffic.  NetTracker tracks this outbound traffic by host.
