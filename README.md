scope
=====

Last version: beta avaible as zip.

Needs NI VISA:

Windows: http://joule.ni.com/nidu/cds/view/p/id/3823/lang/en

MAC OS X: http://joule.ni.com/nidu/cds/view/p/id/3824/lang/en

Linux (redhat/suse) : http://joule.ni.com/nidu/cds/view/p/id/2040/lang/en

Must be installed before running the program!


Calibration: 
Set both inputs to GND and offset to 0V, read the waveform.
If the graph is drawn in center of the screen, then all is ok, if not, save the CSV file. 

If not experiment with the calibration value in order to get it in center.  
Top of the screen = 25, bottom of the screen = 224 for my oscilloscope.
This value is saved to an settings file and read when the program starts. 

If neccesarry it should be possible to get the cal value read automaticaly from the scope. 
