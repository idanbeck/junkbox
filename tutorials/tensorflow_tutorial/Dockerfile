FROM ubuntu:latest

RUN apt-get update && apt-get -y update

RUN apt-get install -y build-essential python3.6 python3-pip python3-dev

RUN pip3 -q install --upgrade pip 

RUN mkdir notebooks

VOLUME ./notebooks /notebooks

COPY ./requirements.txt requirements.txt

RUN pip3 install -r requirements.txt

RUN pip3 install jupyter

# Add Tini. Tini operates as a process subreaper for jupyter. This prevents kernel crashes.
ENV TINI_VERSION v0.6.0
ADD https://github.com/krallin/tini/releases/download/${TINI_VERSION}/tini /usr/bin/tini
RUN chmod +x /usr/bin/tini
ENTRYPOINT ["/usr/bin/tini", "--"]

WORKDIR /notebooks

#CMD ["jupyter", "notebook", "--port=8888", "--no-browser", "--ip=0.0.0.0", "--allow-root"]
CMD ["jupyter", "notebook", "--port=8888", "--ip=0.0.0.0", "--allow-root"]