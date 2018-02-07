def label_creator(cnn_out, filename):
    cnn_out = cnn_out[0]
    index = cnn_out.argmax()

    if float(cnn_out[index]) < 0.95:
        return "Error"
    else:
        f = open(filename, 'r')
        i = 0
        for line in f:
            symbol = line[0: len(line) - 1]
            if (symbol != ""):
                if (i == index):
                    break
                i = i + 1
        f.close()
        return symbol
