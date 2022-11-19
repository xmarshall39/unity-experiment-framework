# UPBS - Unity Playback Suite
Built off the Unity Experiment Framework, UPBS is a collection of data analysis tools which allow for deterministic, frame-by-frame replays of experimental trials in their original environment. Feom there, developers can add additional visualizations and analysis directly in Unity.

This project was recently displayed at the Psychonomic Society 2022 confrence and a digital version can be viewed here:
<img src="media/UPBS_Poster.png" alt="Psychonomic Society 2022 poster">

These tools are still early in development, but this page will receive updates as development progresses.

## Features

### Data Collection
Data collection paradigms and processes are implemented using UXF's source. Minor modifications are made to support Playback features, but implementing UXF is procedurally identical here.
###Playback Environment Generation
Rather than dirtying the experimental environment, UPBS offers the ability to generate copies of said environment that carry over all information relevant to visual stimuli.

### Data Visualization
UPBS provides a straightforward coding framework for adding visualizations extrapolated from collected data

### Additional Data Import
For experimental setups that require the use of external tools for data collection, UPBS allows that data to be introduced to Unity and visualized alongside Unity-side data. Given potential differences in recording rate and data formatting, some unique restrictions may apply to external data.

### Additional Data Export
Since UPBS provides replays with all of the original environmental state information relevant at runtime, it also supports the ability to export additional data during playback. This is especially useful for processes that would compromise frame-rate during experimentation.

## Release Schedule
The first UPBS package release is scheduled to drop January 3rd 2023.

More frequent and detailed progress updates will be detailed on the Development branch.

## Contact
For inquires on the project, feel free to reach out to xmarshall.dev@gmail.com
