import numpy
from matplotlib import pyplot
import matplotlib
import glob
import sys

filename = sys.argv[1]
output_path = sys.argv[2]
img_array = numpy.load(filename)

for i in range(0, len(img_array) - 1):
	pyplot.imshow(img_array[i], cmap="gray")
	img_name = output_path + str(i) + ".png"
	matplotlib.image.imsave(img_name, img_array[i])
	print(img_name + ' was done')