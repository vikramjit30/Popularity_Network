# Popularity_Network

This program generates the popularity network based on given parameters in the config file. After network formation, Kuramoto model is implemented on it and results are stored in output folder.

Parameters:

nodes=30    # Number of nodes in each cluster of network  
inter_edges=200   # Number of edges between clusters
intra_edges=400  # Number of edges within each cluster
clusters=8       # Total number of clusters    
number_of_graphs=50       # Total number of graphs/networks 
coupling_strength=15      # coupling strength to apply Kuramoto Model
coupling_probability=0.99 # probability with which nodes couples
running_time=40           # Time period 
