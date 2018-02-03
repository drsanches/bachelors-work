from PIL import Image

image = Image.open("1.png")
width = image.size[0]
height = image.size[1]
pix = image.load()

f = open("1.txt", "w")

for i in range(height):
	for j in range(width):
		i1 = height - i - 1
		j1 = width - j - 1
		a = pix[j, i][0]
		b = pix[j, i][1]
		c = pix[j, i][2]
		S = 1 - (a + b + c) / 3 / 255
		# if S != 0:
		# 	S = 1
		# else:
		# 	S = 0
		f.write(str(S) + " ")
	f.write("\n")

f.close()
