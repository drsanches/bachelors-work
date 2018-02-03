import sys
from keras.models import model_from_json
from functions import array_functions
from functions import label_functions

try:
	f = open("1111.txt", "w")
	f.write("1111")
	f.close()
	print(111111)
except:
	print("ERROR")