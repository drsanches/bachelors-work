import numpy
from matplotlib import pyplot
import matplotlib
import glob

filename = 'alphanum-hasy-data-X.npy'
img_array = numpy.load(filename)

for i in range(0, len(img_array) - 1):
	pyplot.imshow(img_array[i], cmap="gray")
	img_name = 'X/' + str(i) + ".png"
	matplotlib.image.imsave(img_name, img_array[i])
	print(img_name + ' was done')