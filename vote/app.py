import os
import random
import json
import socket
from flask import Flask, render_template, request, make_response, g
from azure.storage.queue import QueueService, QueueMessageFormat

option_a = os.getenv('OPTION_A', "Cats")
option_b = os.getenv('OPTION_B', "Dogs")
storage_account = os.getenv('AZURE_STORAGE_ACCOUNT')
storage_access_key = os.getenv('AZURE_STORAGE_ACCESS_KEY')
hostname = socket.gethostname()

app = Flask(__name__)

def get_queue():
    if not hasattr(g, 'queue'):
        g.queue = QueueService(account_name=storage_account, account_key=storage_access_key)
        g.queue.create_queue('votes')
        g.queue.encode_function = QueueMessageFormat.text_base64encode
    return g.queue

@app.route("/", methods=['POST','GET'])
def home():
    voter_id = request.cookies.get('voter_id')
    if not voter_id:
        voter_id = hex(random.getrandbits(64))[2:-1]

    vote = None

    if request.method == 'POST':
        queue = get_queue()
        vote = request.form['vote']
        data = json.dumps({'voter_id': voter_id, 'vote': vote})
        queue.put_message('votes', unicode(data))

    resp = make_response(render_template(
        'index.html',
        option_a=option_a,
        option_b=option_b,
        hostname=hostname,
        vote=vote,
    ))
    resp.set_cookie('voter_id', voter_id)
    return resp


if __name__ == "__main__":
    if storage_account is None:
        raise Exception('AZURE_STORAGE_ACCOUNT is not set')
    if storage_access_key is None:
        raise ValueError('AZURE_STORAGE_ACCESS_KEY is not set')

    app.run(host='0.0.0.0', port=80, debug=True, threaded=True)
