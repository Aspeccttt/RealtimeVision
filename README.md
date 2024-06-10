# Data Visualisation in Unity 3D

![Scatterplot using the GPU dataset](https://github.com/Aspeccttt/RealtimeVision/blob/main/Scatterplot%20using%20the%20GPU%20dataset.png?raw=true)

## Overview

This repository contains the Unity 3D prototype developed as part of the thesis "Data Visualisation in Unity 3D" by Ilario Cutajar. The project aims to enhance data visualisation techniques by leveraging Unity 3D to create immersive, interactive, and accessible visual representations of complex datasets. This document provides an overview of the thesis, the research methodology, and instructions on how to set up and use the prototype.

## Thesis Summary

### Introduction

The project addresses the need for advanced visualisation tools capable of handling complex, large-scale datasets. Traditional methods often fall short in terms of interactivity and immersion. Unity 3D, known for its robust capabilities in game development, offers promising solutions for dynamic and interactive data visualisation.

### Literature Review

The literature review covers:
- Theoretical background on data visualisation and visual perception theories.
- Traditional and advanced data visualisation techniques.
- Applications of Unity 3D beyond gaming, particularly in data visualisation.
- Gaps in the literature, specifically in accessibility and immersive technologies.

### Research Methodology

The research employs a mixed-methods approach, integrating both quantitative and qualitative data collection techniques. The Research Onion framework guides the research design, ensuring a comprehensive evaluation of the prototype.

### Results and Discussion

The prototype's effectiveness is analysed through:
- Quantitative metrics such as plotting duration and response times.
- Qualitative observations of user interactions.
The findings demonstrate that Unity 3D significantly enhances data comprehension and user engagement.

### Conclusions and Recommendations

The study concludes that Unity 3D can transform data visualisation by making it more engaging and inclusive. Recommendations for future research and improvements are provided.

## Prototype Description

The Unity 3D-based data visualisation prototype enables users to upload and interact with CSV datasets in a 3D environment. The prototype supports various visualisation types, including scatter plots, line graphs, and histograms. It also logs user interactions for further analysis.

### Features

- **CSV Upload**: Users can upload CSV files for visualisation.
- **Interactive Visualisation**: Users can interact with data points in a 3D space.
- **Real-time Updates**: Visualisations update in real-time based on user interactions.
- **Data Logging**: User interactions are logged for quantitative and qualitative analysis.
- **Accessibility**: The prototype includes features to improve accessibility for users with disabilities.

## Setup Instructions

### Prerequisites

- Unity 3D (version 2020.3 or later)
- Firebase account for data logging

### Installation

1. Clone this repository:
   ```sh
   git clone https://github.com/yourusername/Data-Visualisation-Unity3D.git
   cd Data-Visualisation-Unity3D
   ```
2. Open the project in Unity:
3. Configure Firebase:

- Follow the Firebase setup instructions in the Unity documentation to integrate Firebase with your project.
- Update the DatabaseManager.cs script with your Firebase credentials.
4. Build and run the project in Unity.

### Usage
1. Launch the application.
2. Use the "Upload CSV" button to select and upload a CSV file.
3. Choose the desired columns for visualisation.
4. Interact with the 3D visualisation using the provided controls.
5. View real-time updates and engage with the data.

### Contributing
Contributions are welcome! Please make sure to cite the author in the studies!

### Contact
Please contact at ilariocutajar@gmail.com for any questions or inquiries.
