import sys, json

def get_input():
    return json.load(sys.stdin)

# Json can only serialize or deserialize int, float, str, dict, list, tuple...
def set_output(obj):
    try:
        json.dump(obj, sys.stdout)
    except:
        if hasattr(obj, '__dict__'):  # try to convert class object to serializable object
            json.dump(obj.__dict__, sys.stdout)
        else:
            json.dump(obj, sys.stdout)