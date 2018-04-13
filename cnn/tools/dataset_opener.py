import numpy
from matplotlib import pyplot
import matplotlib
import glob
import sys

filename = sys.argv[1]
output_path = sys.argv[2]
img_array = numpy.load(filename)

for i in range(0, len(img_array) - 1):
	image = img_array[i].reshape(img_array[i].shape[0], img_array[i].shape[1])
	pyplot.imshow(image, cmap="gray")
	img_name = output_path + str(i) + ".png"
	matplotlib.image.imsave(img_name, image)
	print(img_name + ' was done')