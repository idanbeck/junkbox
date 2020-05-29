To create the django project

`sudo docker-compose run web django-admin startproject <name-of-project> .`

Change ownership of files created

`sudo chown -R $USER:$USER .`

To run the server

`docker-compose up`

`docker container ls`

Can remove django project directory at shutdown


Update `settings.py`
```
# Database
# https://docs.djangoproject.com/en/2.2/ref/settings/#databases

DATABASES = {
    'default': {
        'ENGINE': 'django.db.backends.postgresql',
        'NAME': 'postgres',
        'USER': 'postgres',
        'PASSWORD': 'postgres',
        'HOST': 'db',
        'PORT': 5432,
    }
}
```

To create an app, must be in a project

`python manage.py startapp polls`

`sudo docker-compose run web python manage.py migrate`

run this to create migrations - then above to apply them

`sudo docker-compose run web python manage.py makemigrations polls`

`sudo docker-compose run web python manage.py sqlmigrate polls 0001`

`sudo docker-compose run web python manage.py check`

To update models: 

Change your models (in models.py).
Run python manage.py makemigrations to create migrations for those changes
Run python manage.py migrate to apply those changes to the database.

`sudo docker-compose run web python manage.py shell`

`sudo docker-compose run web python manage.py createsuperuser`


Test client
```
python manage.py shell >>>

from django.test.utils import setup_test_environment
setup_test_environment()

from django.test import Client
client = Client()

from django.urls import reverse
response = client.get(reverse('polls:index'))
response.status_code
response.content

response.context['latest_question_list']
```

```
python -c "import django; print(django.__path__)"
cp ~/.local/lib/python3.6/site-packages/django/contrib/admin/templates/admin/base_site.html templates/admin/.
```