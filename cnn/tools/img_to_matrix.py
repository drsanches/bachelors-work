from PIL import Image
import sys

input = sys.argv[1]
output = sys.argv[2]

image = Image.open(input)
width = image.size[0]
height = image.size[1]
pix = image.load()

f = open(output, "w")

for i in range(height):
	for j in range(width):
		a = pix[j, i][0]
		b = pix[j, i][1]
		c = pix[j, i][2]
		S = 1 - (a + b + c) / 3 / 255
		f.write(str(S) + " ")
	f.write("\n")

f.close()
