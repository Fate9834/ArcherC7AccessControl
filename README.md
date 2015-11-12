Archer C7 Access Control
========================

The Archer C7 is a great little router that got excellent reviews. Unfortunately, the
access controls are a pain in the neck to configure. Hence, I wrote a little program
that does that for you.

Use the enclosed XML file to configure the computers you want to have access or not.

* Any computer mentioned in the "hosts" node will be granted access through WiFi.
* Any computer listed as having a schedule, matching a "schedule" node, will have
  a set of rules added to it that limits (or grants) access during a particular time.
* The number of hosts you can add to the Archer C7 is limited (I think it's 16), so
  you might run out of memory quick if you go with the policy=allow option.

What the program will do, in order:

1. Disable all MAC filtering for the Wifi (2.4 GHz and 5 GHz) and remove any previous 
   access controls hosts, schedules and rules.
2. Add all the hosts listed in the hosts section.
3. Add all the schedules.
4. Add all rules that matches hosts to schedules.
5. Add MAC filtering rules for all hosts, whether on schedules or not.
6. Reenable the 2.4Ghz and 5Ghz access policy and MAC filtering.

Note that you can always access the router through an Ethernet cable, even if something
should go wrong.

This should make your life a little easier, when you need to make sure your kids
aren't up in the middle of the night watching Netflix.

_Caveat: Rules can't go over midnight. To block 22:00 to 06:00, you have to create two
rules from 22:00-23:59, and 00:00-06:00. Which also means that anyone will be able
to access the internet between 23:59 and 00:00. I've filed a bug report with the
manufacturer, but surprisingly enough, they have not replied to me._
